#!/bin/bash
cd Help
mv icons ikons
cd html
find . -type f -print0 | xargs -0 sed -i 's/\/icons\//\/ikons\//g'