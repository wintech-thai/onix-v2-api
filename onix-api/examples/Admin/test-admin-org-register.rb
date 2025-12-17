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

### 
apiUrl = "admin-api/AdminUser/org/#{orgId}/action/InviteUser"
param =  {
  UserName: "seubpong.soodlor",
  TmpUserEmail: "hello.seub@please-scan.com",
  Tags: "test,local",
  InvitedBy: "seubpong.mon",
  Roles: [ 'OWNER' ],
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)

apiUrl = "admin-api/AdminUser/org/#{orgId}/action/GetUserCount"
result = make_request(:post, apiUrl, param)
puts(result)
