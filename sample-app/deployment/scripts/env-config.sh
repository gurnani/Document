#!/bin/sh


echo "Configuring environment variables..."

BUILD_DIR="/usr/share/nginx/html"

find $BUILD_DIR -name "*.js" -exec sed -i "s|REACT_APP_API_URL_PLACEHOLDER|${REACT_APP_API_URL:-http://localhost:8080}|g" {} \;
find $BUILD_DIR -name "*.js" -exec sed -i "s|REACT_APP_ENVIRONMENT_PLACEHOLDER|${REACT_APP_ENVIRONMENT:-production}|g" {} \;

echo "Environment configuration complete."
