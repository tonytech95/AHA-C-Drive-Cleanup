# Enable Task Scheduler history
# wevtutil set-log Microsoft-Windows-TaskScheduler/Operational /enabled:true

# Function to get the currently logged-on user
function Get-LoggedOnUser {
    $queryResults = quser
    $queryResults | ForEach-Object {
        if ($_ -match "(\S+)\s+(\S+)\s+(\S+)\s+(\S+)\s+(\S+)\s+(\S+\s+\S+\s+\S+)") {
            $username = $matches[1]
            $state = $matches[4]
            if ($state -eq 'Active') {
                return $username
            }
        }
    }
    return $null
}

# Get the currently logged-on user
$UserName = Get-LoggedOnUser

if ($UserName) {
    $TaskName = "${UserName}_CleanUpTask"
    # Trigger the scheduled task
    Start-ScheduledTask -TaskName $TaskName
    
    Start-Sleep -Seconds 10
}
else {
    Write-Output "No active user session found."
}

# Disable Task Scheduler history
# wevtutil set-log Microsoft-Windows-TaskScheduler/Operational /enabled:false