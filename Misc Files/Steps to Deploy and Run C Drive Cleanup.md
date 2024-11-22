# Steps to Deploy and Run C Drive Cleanup

## Steps

### Step 1: Create a Group Policy Object (GPO)

1. **Open Group Policy Management**:
   - On your domain controller, open the Group Policy Management Console (GPMC).

2. **Create a New GPO**:
   - Right-click on the Organizational Unit (OU) where you want to apply the policy and select **Create a GPO in this domain, and Link it here...**.
   - Name the GPO (e.g., "Deploy CheckAndClean").

### Step 2: Configure the GPO to Copy the Content

1. **Edit the GPO**:
   - Right-click the newly created GPO and select **Edit**.

2. **Navigate to File Preferences**:
   - Go to **Computer Configuration** > **Preferences** > **Windows Settings** > **Files**.

3. **Create a New File Action**:
   - Right-click on **Files**, select **New** > **File**.
   - In the **Action** dropdown, select **Update**.
   - In the **Source File(s)** field, enter the path to the content on the network drive (e.g., `\\NetworkDrive\Path\aha drive clean up\*`).
   - In the **Destination File** field, enter the path to the destination folder on the user's machine (e.g., `C:\ProgramData\C Drive Cleanup\`).

### Step 3: Configure the GPO to Run a Script at User Login

1. **Navigate to Scripts (Logon/Logoff) Preferences**:
   - Go to **User Configuration** > **Policies** > **Windows Settings** > **Scripts (Logon/Logoff)**.

2. **Create a New Logon Script**:
   - Double-click on **Logon**, then click **Add**.
   - In the **Script Name** field, enter the path to the PowerShell script (e.g., `\\NetworkDrive\Path\createusertask.ps1`).

3. **Configure the Script Parameters** (if needed):
   - The script defaults to First Monday of month. If need to modify, enter `-WeekDay "Monday" -WekkofMonth "1"` (which translate to First Monday) in the **Script Parameters** field.

### Step 4: Apply and Test the GPO

1. **Link the GPO**:
   - Ensure the GPO is linked to the correct OU where the target users are located.

2. **Force Group Policy Update**:
   - On a target machine, run `gpupdate /force` to apply the new GPO.

3. **Verify the Deployment**:
   - Check that the script runs at user login and performs the intended actions.

## Summary

By following these steps, you can use Group Policy to deploy the executable to a hidden folder on user machines and create a scheduled task that runs on the first Monday of each month. This approach ensures that the executable is deployed and scheduled without user intervention. Additionally, you can run a PowerShell script (`createusertask.ps1`) at user login to automate further tasks.
