create self-signed cert in powershell:
$cert = New-SelfSignedCertificate -certstorelocation cert:\localmachine\my -dns my-k8s.io
$pwd = ConvertTo-SecureString -String "Secret" -Force -AsPlainText
$certpath = "Cert:\localmachine\my\$($cert.Thumbprint)"
Export-PfxCertificate -Cert $certpath -FilePath c:\Web.pfx -Password $pwd

add-migration -Context AppDbContext
update-database -Context AppDbContext

https://docs.fluentvalidation.net/en/latest/built-in-validators.html