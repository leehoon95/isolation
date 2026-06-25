ssh -i gcp_key  ganghoon95@8.235.7.143 "rm -rf /var/www/downloads/StandaloneWindows64"
scp -r -i gcp_key StandaloneWindows64 ganghoon95@8.235.7.143:/var/www/downloads
