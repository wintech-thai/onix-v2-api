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
bankAccountId = '5f38c8e3-ad9c-43c1-8c61-88dfe0dcc483'

### 
#apiUrl = "admin-api/AdminBankAccount/org/#{orgId}/action/EnableBankAccountById/#{bankAccountId}"
apiUrl = "admin-api/AdminBankAccount/org/#{orgId}/action/DisableBankAccountById/#{bankAccountId}"
param = nil

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)
