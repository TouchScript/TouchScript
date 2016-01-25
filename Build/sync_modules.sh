#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
MODULES=$(cd "$DIR/../Modules/" && pwd)

if [ -d "$MODULES" ]; then
	for i in $(ls -d "$MODULES/"*/); do 
		FOLDER="${i%%/}"
		"$DIR/utils/sync_module.sh" "$FOLDER" 
	done
fi