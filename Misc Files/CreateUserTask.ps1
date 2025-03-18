# Variables for the task name, day, and time
param (
  [string]$WeekDay = "Monday", # Default to Monday
  [string]$WeekofMonth = "1"   # Default to first week
)

$UserName = $env:USERNAME
$DomainName = $env:USERDOMAIN
$TaskName = "${UserName}_CleanUpTask"
$ExecutablePath = "C:\ProgramData\C Drive Cleanup\AHA C Drive Cleanup.exe" # Path to the executable

# Check if the task already exists
$taskExists = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue

if ($taskExists -and $taskExists.Description -match "Task for $UserName to clean Downloads and Recycle Bin on the $weekDay of Week $WeekofMonth of the month") {
  Write-Output "Task $TaskName already exists. Skipping creation."
}
else {
  # XML for the scheduled task
  $TaskXml = @"
<?xml version="1.0" encoding="UTF-16"?>
<Task version="1.3" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
  <RegistrationInfo>
    <Date>$(Get-Date -Format 'yyyy-MM-ddTHH:mm:ss')</Date>
    <Author>$DomainName\$UserName</Author>
    <Description>Task for $UserName to clean Downloads and Recycle Bin on the $weekDay of Week $WeekofMonth of the month</Description>
  </RegistrationInfo>
  <Triggers>
    <CalendarTrigger>
      <StartBoundary>2024-01-01T09:00:00</StartBoundary>
      <Enabled>true</Enabled>
      <ScheduleByMonthDayOfWeek>
        <Weeks>
          <Week>$WeekofMonth</Week>
        </Weeks>
        <DaysOfWeek>
          <$WeekDay />
        </DaysOfWeek>
        <Months>
          <January />
          <February />
          <March />
          <April />
          <May />
          <June />
          <July />
          <August />
          <September />
          <October />
          <November />
          <December />
        </Months>
      </ScheduleByMonthDayOfWeek>
    </CalendarTrigger>
  </Triggers>
  <Principals>
    <Principal id="Author">
      <UserId>$DomainName\$UserName</UserId>
      <LogonType>InteractiveToken</LogonType>
      <RunLevel>LeastPrivilege</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
    <AllowHardTerminate>false</AllowHardTerminate>
    <StartWhenAvailable>true</StartWhenAvailable>
    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>
    <IdleSettings>
      <Duration>PT5M</Duration>
      <WaitTimeout>PT1H</WaitTimeout>
      <StopOnIdleEnd>false</StopOnIdleEnd>
      <RestartOnIdle>false</RestartOnIdle>
    </IdleSettings>
    <AllowStartOnDemand>true</AllowStartOnDemand>
    <Enabled>true</Enabled>
    <Hidden>false</Hidden>
    <RunOnlyIfIdle>false</RunOnlyIfIdle>
    <WakeToRun>false</WakeToRun>
    <ExecutionTimeLimit>PT1H</ExecutionTimeLimit>
    <Priority>7</Priority>
    <RestartOnFailure>
      <Interval>PT1M</Interval>
      <Count>3</Count>
    </RestartOnFailure>
  </Settings>
  <Actions Context="Author">
    <Exec>
      <Command>"$ExecutablePath"</Command>
    </Exec>
  </Actions>
</Task>
"@

  #Unregister the task first if it already exists
  Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false


  # Register the scheduled task using the XML content directly
  Register-ScheduledTask -TaskName $TaskName -Xml $TaskXml

  # Output a success message
  Write-Output "Task $TaskName created successfully."
}