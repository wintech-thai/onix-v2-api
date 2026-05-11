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
apiKeyId = "59745bfb-a48a-4277-a968-3c7c9060d1cd" # เปลี่ยนเป็น API Key ID ที่ต้องการทดสอบ

### 
apiUrl = "admin-api/AdminApiKey/org/#{orgId}/action/GetApiKeyById/#{apiKeyId}"
param = nil
token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:get, apiUrl, param)
puts(result)
