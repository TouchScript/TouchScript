if [[ $# -eq 0 ]] ; then
    printf "\e[31mUsage: clean.sh <project folder>\e[39m\n"
    exit 0
fi

if [ ! -d "${1}" ]; then
    printf "\e[31mError! Project folder '${1}' does not exist.\e[39m\n"
    exit 0
fi

printf "\n\e[32mCleaning project $1.\e[39m\n"
rm -rf "${1}/Library"
rm -rf "${1}/obj"
rm -rf "${1}/Temp"