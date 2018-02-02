@echo off
java -jar signapk.jar platform.x509.pem platform.pk8 %~n1%~x1 %~n1_signed%~x1
Pause
EXIT