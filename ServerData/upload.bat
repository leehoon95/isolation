ssh -i gcp_key  ganghoon95@34.11.242.48 "rm -rf /var/www/uploads/StandaloneWindows64"
scp -r -i gcp_key StandaloneWindows64 ganghoon95@34.11.242.48:/var/www/uploads