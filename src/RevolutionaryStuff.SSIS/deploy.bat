copy /Y bin\%1\RevolutionaryStuff.SSIS.dll "C:\Program Files (x86)\Microsoft SQL Server\130\DTS\PipelineComponents"
copy /Y bin\%1\RevolutionaryStuff.SSIS.pdb "C:\Program Files (x86)\Microsoft SQL Server\130\DTS\PipelineComponents"
copy /Y bin\%1\RevolutionaryStuff.SSIS.extensions.xml "C:\Program Files (x86)\Microsoft SQL Server\130\DTS\UpgradeMappings"
"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7 Tools\gacutil.exe" -if bin\%1\RevolutionaryStuff.SSIS.dll
