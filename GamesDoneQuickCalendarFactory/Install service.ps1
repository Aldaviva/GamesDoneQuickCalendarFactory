$binaryPathName = Resolve-Path(join-path $PSScriptRoot "GamesDoneQuickCalendarFactory.exe")

New-Service -Name "GamesDoneQuickCalendarFactory" -DisplayName "Games Done Quick Calendar Factory" -Description "Serve GDQ ICS file." -BinaryPathName $binaryPathName.Path -DependsOn Tcpip