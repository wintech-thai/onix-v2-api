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
userId = '1418613b-072e-4160-8fe0-d5b5f63e2a65'

### 
apiUrl = "admin-api/AdminUser/org/#{orgId}/action/GetForgotPasswordLink/#{userId}"
param = nil

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:get, apiUrl, param)
puts(result)
