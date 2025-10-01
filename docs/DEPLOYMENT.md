# EduTrack Deployment Guide

## Windows IIS Deployment

### Prerequisites

1. **Windows Server** with IIS installed
2. **.NET 8 Hosting Bundle** installed
3. **SQL Server** or **SQLite** (for file-based deployment)

### Step-by-Step Deployment

1. **Publish the Application**
   ```powershell
   .\scripts\publish-iis.ps1
   ```

2. **Install .NET 8 Hosting Bundle**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Run the installer as Administrator
   - Restart IIS after installation

3. **Create IIS Site**
   - Open IIS Manager
   - Right-click "Sites" → "Add Website"
   - Site name: `EduTrack`
   - Physical path: `C:\inetpub\wwwroot\EduTrack` (or your publish directory)
   - Port: `80` (or your preferred port)
   - Click "OK"

4. **Configure Application Pool**
   - Select the EduTrack site
   - Click "Basic Settings" → "Select"
   - Create new Application Pool: `EduTrackPool`
   - .NET CLR Version: "No Managed Code"
   - Managed Pipeline Mode: "Integrated"
   - Click "OK"

5. **Set Permissions**
   - Right-click the site folder → "Properties" → "Security"
   - Add "IIS_IUSRS" with "Full Control"
   - Add "IIS AppPool\EduTrackPool" with "Full Control"

6. **Configure SSL (Optional)**
   - Install SSL certificate
   - Bind HTTPS on port 443
   - Redirect HTTP to HTTPS

### Configuration Files

Update `appsettings.Production.json`:
```json
{
  "Database": {
    "Provider": "SqlServer",
    "ConnectionStrings": {
      "SqlServer": "Server=YOUR_SERVER;Database=EduTrack;Trusted_Connection=True;TrustServerCertificate=True"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

## Linux Deployment

### Prerequisites

1. **Linux Server** (Ubuntu 22.04+ recommended)
2. **.NET 8 Runtime** installed
3. **PostgreSQL** or **SQLite** (for file-based deployment)

### Step-by-Step Deployment

1. **Publish the Application**
   ```bash
   chmod +x scripts/publish-linux.sh
   ./scripts/publish-linux.sh
   ```

2. **Install .NET 8 Runtime**
   ```bash
   # Ubuntu/Debian
   wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
   sudo dpkg -i packages-microsoft-prod.deb
   sudo apt-get update
   sudo apt-get install -y dotnet-runtime-8.0
   ```

3. **Deploy Application**
   ```bash
   sudo mkdir -p /opt/edutrack
   sudo cp -r ./publish/linux/* /opt/edutrack/
   sudo chown -R www-data:www-data /opt/edutrack
   sudo chmod -R 755 /opt/edutrack
   ```

4. **Create Systemd Service**
   ```bash
   sudo cp docs/edutrack.service /etc/systemd/system/
   sudo systemctl daemon-reload
   sudo systemctl enable edutrack
   sudo systemctl start edutrack
   sudo systemctl status edutrack
   ```

5. **Configure Nginx (Optional)**
   ```bash
   sudo apt-get install nginx
   sudo cp docs/nginx.conf /etc/nginx/sites-available/edutrack
   sudo ln -s /etc/nginx/sites-available/edutrack /etc/nginx/sites-enabled/
   sudo nginx -t
   sudo systemctl restart nginx
   ```

### Configuration Files

Update `appsettings.Production.json`:
```json
{
  "Database": {
    "Provider": "Postgres",
    "ConnectionStrings": {
      "Postgres": "Host=localhost;Database=edutrack;Username=postgres;Password=YOUR_PASSWORD"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

## Database Setup

### SQLite (File-based)

No additional setup required. The database file will be created automatically at `App_Data/edutrack.db`.

### SQL Server

1. **Install SQL Server**
2. **Create Database**
   ```sql
   CREATE DATABASE EduTrack;
   ```
3. **Run Migrations**
   ```bash
   dotnet ef database update --context AppDbContext
   ```

### PostgreSQL

1. **Install PostgreSQL**
   ```bash
   sudo apt-get install postgresql postgresql-contrib
   ```
2. **Create Database and User**
   ```sql
   CREATE DATABASE edutrack;
   CREATE USER edutrack_user WITH PASSWORD 'your_password';
   GRANT ALL PRIVILEGES ON DATABASE edutrack TO edutrack_user;
   ```
3. **Run Migrations**
   ```bash
   dotnet ef database update --context AppDbContext
   ```

## Environment Configuration

### Production Settings

Create `appsettings.Production.json`:
```json
{
  "Database": {
    "Provider": "SqlServer",
    "ConnectionStrings": {
      "SqlServer": "Server=YOUR_SERVER;Database=EduTrack;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
    }
  },
  "Paths": {
    "StorageRoot": "/var/edutrack/storage"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": "Warning",
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/edutrack/edutrack-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

## Monitoring and Maintenance

### Log Files

- **Windows**: `C:\inetpub\wwwroot\EduTrack\App_Data\logs\`
- **Linux**: `/var/log/edutrack/`

### Health Checks

Access the health check endpoint:
- `https://your-domain.com/health`

### Backup Strategy

1. **Database Backup**
   ```bash
   # SQL Server
   sqlcmd -S YOUR_SERVER -d EduTrack -Q "BACKUP DATABASE EduTrack TO DISK = 'C:\Backup\EduTrack.bak'"
   
   # PostgreSQL
   pg_dump -h localhost -U edutrack_user edutrack > edutrack_backup.sql
   ```

2. **File Storage Backup**
   ```bash
   # Backup storage directory
   tar -czf edutrack_storage_backup.tar.gz /opt/edutrack/wwwroot/storage/
   ```

### Updates

1. **Stop the service**
   ```bash
   # Windows
   iisreset /stop
   
   # Linux
   sudo systemctl stop edutrack
   ```

2. **Backup current version**
3. **Deploy new version**
4. **Run database migrations**
   ```bash
   dotnet ef database update --context AppDbContext
   ```
5. **Start the service**
   ```bash
   # Windows
   iisreset /start
   
   # Linux
   sudo systemctl start edutrack
   ```

## Troubleshooting

### Common Issues

1. **Database Connection Errors**
   - Check connection string
   - Verify database server is running
   - Check firewall settings

2. **Permission Errors**
   - Verify IIS application pool identity
   - Check folder permissions
   - Ensure write access to App_Data and storage folders

3. **SSL Certificate Issues**
   - Verify certificate is valid
   - Check certificate binding
   - Ensure certificate is trusted

### Log Analysis

Check application logs for errors:
```bash
# Windows
Get-Content "C:\inetpub\wwwroot\EduTrack\App_Data\logs\edutrack-*.txt" | Select-String "ERROR"

# Linux
grep "ERROR" /var/log/edutrack/edutrack-*.txt
```

### Performance Monitoring

Monitor key metrics:
- Response times
- Memory usage
- Database connection pool
- Disk space usage

## Security Considerations

### Production Security

1. **Use HTTPS** in production
2. **Change default passwords**
3. **Enable Windows Authentication** (if using SQL Server)
4. **Restrict database access** to application only
5. **Regular security updates**
6. **Firewall configuration**
7. **Backup encryption**

### File Upload Security

- Validate file types
- Scan for malware
- Limit file sizes
- Store files outside web root (if possible)

---

For additional support, refer to the main README.md or create an issue in the repository.
