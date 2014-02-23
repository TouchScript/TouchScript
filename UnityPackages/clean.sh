#!bash

printf "\e[32mCleaning Projects...\e[39m\n"
for i in $(ls -d */); do 
	echo $i
	rm -rf "${i}Library"
	rm -rf "${i}obj"
	rm -rf "${i}Temp"
done