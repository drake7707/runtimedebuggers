cd /d %~dp0

%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe ".\RunTimeDebuggers\RunTimeDebuggers3.5x64.csproj" /t:Rebuild /p:TargetFrameworkVersion=v3.5
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe ".\RunTimeDebuggers\RunTimeDebuggers3.5x86.csproj" /t:Rebuild /p:TargetFrameworkVersion=v3.5
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe ".\RunTimeDebuggers\RunTimeDebuggers4.0x64.csproj" /t:Rebuild
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe ".\RunTimeDebuggers\RunTimeDebuggers4.0x86.csproj" /t:Rebuild