> Tasks to automate Dynamics 365 post-deployment steps

## 1. Tasks

### 1.1 Data transfer
Transfer configuration data between environments. Code adapted from [ConfigDataMover](https://github.com/lucasalexander/AlexanderDevelopment.ConfigDataMover).

#### 1.1.1 Export to file
Runs a given fetchXml query to retrieve data from CRM and store it as `json` at a given directory.

#### 1.1.2 Import to CRM
Reads data as `json` from a given directory and imports it into CRM.

### 1.2 Duplicate detection rules
Establish which duplicate detection rules are published before a deployment, and then republish them after a deployment.

#### 1.2.1 Get published duplicate detection rules
Gets all published duplicate detection rules in CRM and stores them as a `json` file.

#### 1.2.2 Publish duplicate detection rules
Reads a `json` file to determine which duplicate detection rules should be republished and republishes them.

### 1.3 Deactivate forms
Deactivates all forms in CRM matching any of the ids given as a comma separated string.

### 1.4 Update SLA
Conditionally sets an SLA as being active and/or default. Can also retrieve business hours (calendar) and assign it to the SLA.

### 1.5 Assign workflows to user
Assigns all workflows in CRM to a given user.

## 2. Usage
