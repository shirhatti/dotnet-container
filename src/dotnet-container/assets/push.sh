#!/bin/bash
set -e
set -u

export REGISTRY=demo48.azurecr.io

if [ -z "$REGISTRY" ]
    then
        echo ERROR: Environment variable REGISTRY not defined
        exit 1
fi

if [ -z "$DOCKER_USERNAME" ]
  then 
        echo ERROR: DOCKER_USERNAME not defined. 
        exit 1
fi

if [ -z "$DOCKER_PASSWORD" ]
  then 
        echo ERROR: DOCKER_PASSWORD not defined. 
        exit 1
fi

# while getopts "p:" OPTION; do
#   case $OPTION in
#     p )
#       APP_DIR=$OPTARG
#       ;;
#     ? ) 
#       echo "Usage: push-image.sh -p <PATH>" >&2
#       exit 1
#   esac
# done
# shift "$(($OPTIND -1))"
echo "PUBLISH_DIR" $PUBLISH_DIR
APP_DIR=$PUBLISH_DIR
repo=$REPO
DOCKER_IMAGE_TAG=run
NATIVE_APP_TAG=app
CURL_USER_ARGS="--user $DOCKER_USERNAME:$DOCKER_PASSWORD"
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

cd $APP_DIR
TEMP_DIR=".temp"

if [ -d "$TEMP_DIR" ]
 then 
     rm -rf $TEMP_DIR
fi

# Prepare app: no docker required
# OSX doesn't have sha256sum so alias it i
# stat is -f instead of -c for OSX

echo Pushing app: $REGISTRY/$repo:app
echo oras push $REGISTRY/$repo:$NATIVE_APP_TAG -u $DOCKER_USERNAME -p $DOCKER_PASSWORD .
oras_manifest_digest=$(oras push $REGISTRY/$repo:$NATIVE_APP_TAG -u $DOCKER_USERNAME -p $DOCKER_PASSWORD . | sed -n 's/.*\(sha256:.*\)/\1/p')

mkdir $TEMP_DIR
cd $TEMP_DIR

# Get app manifest
curl $CURL_USER_ARGS -sH "Accept: application/vnd.oci.image.manifest.v1+json" \
        "https://$REGISTRY/v2/$repo/manifests/$oras_manifest_digest" > oras_manifest.json


app_diff_id=$(cat oras_manifest.json | jq -r '.layers[0].annotations["io.deis.oras.content.digest"]')
app_size=$(cat oras_manifest.json | jq -r '.layers[0].size')
app_digest=$(cat oras_manifest.json | jq -r '.layers[0].digest')

##
# Demo Main Part: no docker required
##
echo "Build Image using: $REGISTRY/$repo:base"
# Get base manifest and config
curl $CURL_USER_ARGS  -sH "Accept: application/vnd.docker.distribution.manifest.v2+json" \
        "https://$REGISTRY/v2/$repo/manifests/base" > base_manifest.json
config_digest=$(cat base_manifest.json | jq -r .config.digest)
curl $CURL_USER_ARGS -s -L "https://$REGISTRY/v2/$repo/blobs/$config_digest" > base_config.json

# Modify config file for the new layer
cat base_config.json | jq -c ".rootfs.diff_ids += [\"$app_diff_id\"]" > app_config.json
app_config_size=$(stat -f "%p" app_config.json)
app_config_digest="sha256:$(shasum -a 256 app_config.json | cut -d " " -f1)"

# Modify manifest file for the new layer
cat base_manifest.json |
    jq ".config.size = $app_config_size" |
    jq ".config.digest = \"$app_config_digest\"" |
    jq ".layers += [{\"mediaType\": \"application/vnd.docker.image.rootfs.diff.tar.gzip\",\"size\": $app_size,\"digest\":\"$app_digest\"}]" \
    > app_manifest.json

# Upload config file
echo "Pushing image: $REGISTRY/$repo/$DOCKER_IMAGE_TAG"
upload_url=$(curl $CURL_USER_ARGS -sIXPOST "https://$REGISTRY/v2/$repo/blobs/uploads/" | grep "Location: " | cut -d " " -f2 | tr -d "\r")
curl $CURL_USER_ARGS  --upload-file app_config.json "https://$REGISTRY$upload_url&digest=$app_config_digest"

# Push image (push manifest)
curl $CURL_USER_ARGS -XPUT -d @app_manifest.json \
        -H "Content-Type: application/vnd.docker.distribution.manifest.v2+json" \
        "https://$REGISTRY/v2/$repo/manifests/$DOCKER_IMAGE_TAG"

#clean up
cd .. 
#rm -rf $TEMP_DIR

##
# End Of Demo Main Part
##

# Pull and run the merged image
echo "Commands"
echo "--------"
#echo "Native APP  : dotnet start $REGISTRY/$repo:$NATIVE_APP_TAG" 
echo "Docker Image: docker run --rm -it $REGISTRY/$repo:$DOCKER_IMAGE_TAG" 
