
# 审计日志功能可以记录用户的操作行为，帮助管理员进行安全审计和问题排查。通过配置 "AuditLog" 部分，可以指定是否启用审计日志以及需要记录的操作类型。
"AuditLog": {
  "IsEnabled": true,
  "RecordOperations": [ "Login", "Add", "Edit", "Delete", "Authorize", "Upload" ]
}

- 只记录增删改： "RecordOperations": [ "Add", "Edit", "Delete" ]
- 记录所有关键操作（不含查询）： "RecordOperations": [ "Login", "Add", "Edit", "Delete", "Authorize", "Upload" ]
- 记录全部操作（含查询）： "RecordOperations": [ "Login", "Logout", "View", "Add", "Edit", "Delete", "Authorize", "Upload", "Export" ]
- 留空数组则记录所有： "RecordOperations": []