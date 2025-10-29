# Together Application - Deployment Checklist

Use this checklist to ensure a smooth deployment process.

## Pre-Deployment

### Code Preparation
- [ ] All features tested and working
- [ ] All unit tests passing
- [ ] Integration tests passing
- [ ] Code reviewed and approved
- [ ] No debug code or console logs in production code
- [ ] All TODO comments resolved or documented
- [ ] Database migrations tested

### Version Management
- [ ] Version number updated in `Together.csproj`
- [ ] Assembly version updated
- [ ] File version updated
- [ ] ClickOnce revision incremented (if applicable)
- [ ] Release notes prepared
- [ ] CHANGELOG.md updated

### Configuration
- [ ] Production connection strings configured
- [ ] API keys and secrets secured
- [ ] Logging configuration verified
- [ ] Error handling tested
- [ ] Performance optimizations enabled

### Assets and Branding
- [ ] Application icon created and added (`Together\Assets\together.ico`)
- [ ] Splash screen ready (if applicable)
- [ ] About dialog information updated
- [ ] Copyright information current
- [ ] License file included

## Build Process

### Clean Build
- [ ] Solution cleaned (`dotnet clean`)
- [ ] All dependencies restored (`dotnet restore`)
- [ ] Release configuration selected
- [ ] Build completed without warnings
- [ ] Output directory verified

### Testing on Clean Machine
- [ ] Tested on Windows 10
- [ ] Tested on Windows 11
- [ ] Tested without .NET runtime installed (standalone)
- [ ] Tested with limited user permissions
- [ ] All features functional
- [ ] No missing dependencies

## Deployment Method Selection

### Option A: ClickOnce Deployment
- [ ] Publish URL configured
- [ ] Update settings configured
- [ ] Minimum required version set
- [ ] Prerequisites specified
- [ ] Manifest signed (production)
- [ ] Published successfully
- [ ] Setup.exe tested
- [ ] Update mechanism tested

### Option B: Standalone Installer
- [ ] Self-contained publish completed
- [ ] Single file option configured (if desired)
- [ ] Runtime identifier correct (win-x64)
- [ ] Inno Setup script configured
- [ ] Installer compiled successfully
- [ ] Installer tested on clean machine
- [ ] Uninstaller tested

## Security

### Code Signing
- [ ] Code signing certificate obtained
- [ ] Executable signed
- [ ] Installer signed (if applicable)
- [ ] Manifest signed (ClickOnce)
- [ ] Signature verified

### Security Review
- [ ] No hardcoded credentials
- [ ] Secure token storage implemented
- [ ] TLS 1.2+ enforced
- [ ] Input validation in place
- [ ] SQL injection prevention verified
- [ ] XSS prevention verified (if applicable)

## Documentation

### User Documentation
- [ ] Installation guide created
- [ ] User manual updated
- [ ] Quick start guide available
- [ ] FAQ updated
- [ ] Troubleshooting guide prepared
- [ ] System requirements documented

### Technical Documentation
- [ ] Deployment guide complete
- [ ] Architecture documentation current
- [ ] API documentation updated (if applicable)
- [ ] Database schema documented
- [ ] Configuration guide available

## Distribution

### Package Preparation
- [ ] Installer/setup files ready
- [ ] README.txt included
- [ ] LICENSE.txt included
- [ ] Release notes included
- [ ] System requirements listed

### Upload and Distribution
- [ ] Files uploaded to distribution server
- [ ] Download links tested
- [ ] Update server configured (ClickOnce)
- [ ] CDN configured (if applicable)
- [ ] Backup of previous version created

## Post-Deployment

### Verification
- [ ] Download link accessible
- [ ] Installation successful
- [ ] Application launches correctly
- [ ] All features working
- [ ] Updates working (ClickOnce)
- [ ] Telemetry/analytics working (if implemented)

### Communication
- [ ] Release announcement prepared
- [ ] Users notified of new version
- [ ] Support team briefed
- [ ] Known issues documented
- [ ] Feedback channels ready

### Monitoring
- [ ] Error logging monitored
- [ ] User feedback collected
- [ ] Performance metrics reviewed
- [ ] Update adoption tracked (ClickOnce)
- [ ] Support tickets monitored

## Rollback Plan

### Preparation
- [ ] Previous version backed up
- [ ] Rollback procedure documented
- [ ] Database rollback plan ready (if schema changed)
- [ ] Communication plan for rollback

### Rollback Triggers
- [ ] Critical bugs identified
- [ ] Security vulnerabilities discovered
- [ ] Performance degradation detected
- [ ] High volume of user complaints

## Sign-Off

### Approvals
- [ ] Development team approval
- [ ] QA team approval
- [ ] Product owner approval
- [ ] Security team approval (if applicable)

### Final Steps
- [ ] Deployment date/time confirmed
- [ ] Maintenance window scheduled (if needed)
- [ ] Support team on standby
- [ ] Monitoring alerts configured

---

## Deployment Completed

**Date:** _______________

**Version:** _______________

**Deployed By:** _______________

**Notes:**
_______________________________________________
_______________________________________________
_______________________________________________

## Post-Deployment Review (24-48 hours after deployment)

- [ ] No critical issues reported
- [ ] User feedback positive
- [ ] Performance metrics acceptable
- [ ] Error rates normal
- [ ] Update adoption rate acceptable (ClickOnce)

**Review Date:** _______________

**Reviewed By:** _______________

**Status:** ☐ Success  ☐ Issues Found  ☐ Rollback Required

**Notes:**
_______________________________________________
_______________________________________________
_______________________________________________
