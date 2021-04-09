rm Builds/Linux_Mac.zip
rm Builds/Windows.zip
zip -r Builds/Linux_Mac.zip Builds/Linux_Mac
zip -r Builds/Windows.zip Builds/Windows
git add *
git commit -m $1
git push origin main
