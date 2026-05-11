#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = "global"
keyFile = ".token"
adminUserId = "0927e903-6548-4c87-bd81-9e3688279b75" # เปลี่ยนเป็น AdminUserId ที่ต้องการทดสอบ

### 
apiUrl = "admin-api/AdminUser/org/#{orgId}/action/UpdateUserById/#{adminUserId}"
param = {
  Tags: "Test, Update",
  Roles: ["Admin", "User", "OWNER"]
}
token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)
