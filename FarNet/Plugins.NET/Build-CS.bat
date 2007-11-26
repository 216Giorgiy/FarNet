
:: Synopsis : Build Far.Net plugin library from CS source
:: Example  : Build-CS MyPlugin.cs
:: Output   : MyPlugin.dll

set dotnet=%WINDIR%\Microsoft.NET\Framework\v2.0.50727
set farlib="%FARHOME%\lib\FarNetIntf.dll"
%dotnet%\csc /reference:%farlib% /target:library %*
