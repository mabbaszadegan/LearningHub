# Check if there are groups in the database
$connectionString = "Data Source=src/EduTrack.WebApp/App_Data/EduTrack.db"
$query = "SELECT COUNT(*) as GroupCount FROM StudentGroups"

Write-Host "Checking groups in database..."
Write-Host "Connection String: $connectionString"

# Try to use sqlite3 if available
try {
    $result = sqlite3 "$connectionString" "$query"
    Write-Host "Groups count: $result"
} catch {
    Write-Host "sqlite3 not available, trying alternative method..."
    
    # Alternative: Check if database file exists
    $dbPath = "src/EduTrack.WebApp/App_Data/EduTrack.db"
    if (Test-Path $dbPath) {
        Write-Host "Database file exists at: $dbPath"
        $fileSize = (Get-Item $dbPath).Length
        Write-Host "Database file size: $fileSize bytes"
    } else {
        Write-Host "Database file not found at: $dbPath"
    }
}
