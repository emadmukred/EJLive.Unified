# EJLive Pull Request Template

## 1. Linked Issue / Track
- Issue: #
- Track ID:
- Command file:

## 2. Scope
Allowed scope for this PR:
- [ ] Source Truth / Baseline
- [ ] Build / Project References
- [ ] Client.Service
- [ ] Server Backend / Ingestion
- [ ] Core / Protocol
- [ ] Journal Parser
- [ ] XFS / Vendor Correlation
- [ ] Database / Migrations
- [ ] Tests / Verification
- [ ] Documentation only

## 3. What Changed
List only the actual changes made:
- 

## 4. Files Modified
- 

## 5. Files Added
- 

## 6. Files Removed or Deprecated
- 

## 7. Safety Rules Confirmation
- [ ] I did not rewrite the project from scratch.
- [ ] I did not delete existing working code without replacement and documented rollback.
- [ ] I did not promote reference/legacy code directly into compile without adapter and tests.
- [ ] I did not put long-running execution logic inside WinForms UI event handlers.
- [ ] I did not add local analytics dashboards to the client service.
- [ ] I did not add arbitrary shell execution.
- [ ] I did not disable Defender, firewall, EDR, logging, or audit.
- [ ] I did not log passwords, secrets, card numbers, or account numbers.

## 8. Architecture Impact
Describe impact on layers:
- Client.Service:
- Server:
- Core:
- Data:
- UI:
- Tests/Verification:

## 9. Journal / XFS Impact
- Vendor affected: NCR / GRG / Wincor / Diebold / Hyosung / Generic / None
- Parser affected:
- XFS adapter affected:
- Raw evidence preserved: Yes / No / N/A
- Transaction boundary parsing affected: Yes / No / N/A

## 10. Security Impact
- Remote command impact: Yes / No
- RDP/Firewall/Registry/Password impact: Yes / No
- Requires RBAC/signature/audit/rollback: Yes / No / N/A
- Secrets redacted: Yes / No / N/A

## 11. Commands Executed
Paste exact commands and results:

```powershell
dotnet --info
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore
dotnet build .\EJLive.Unified.slnx --no-restore -m:1 /p:BuildInParallel=false -v:m
```

## 12. Test Evidence
- [ ] Unit tests added or updated.
- [ ] Integration/verification updated where needed.
- [ ] Fixtures added for parser/XFS changes where needed.
- [ ] No tests required because documentation-only change.

## 13. Acceptance Criteria
- [ ] Restore succeeds.
- [ ] Build succeeds.
- [ ] Tests succeed.
- [ ] Verification succeeds.
- [ ] UI does not block backend execution.
- [ ] Client.Service remains headless.
- [ ] Server/dashboard reads snapshots, not direct long-running work.

## 14. Known Limitations
- 

## 15. Rollback Plan
Explain how to revert this safely:
- 
