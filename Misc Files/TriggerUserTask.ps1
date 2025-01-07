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

    $task = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
    if ($task) {
        # Trigger the scheduled task
        Start-ScheduledTask -TaskName $TaskName
        Write-Output "Task '$taskName' has been triggered."

        Start-Sleep -Seconds 10
    } else {
        Write-Output "Task '$taskName' not found."
    }

}
else {
    Write-Output "No active user session found."
}

# Disable Task Scheduler history
# wevtutil set-log Microsoft-Windows-TaskScheduler/Operational /enabled:false