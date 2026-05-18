#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
keyFile = ".token"
bankAccountId = 'a0ebc677-6aa3-4454-9e7e-ee1aa6361e4b'

### 
apiUrl = "admin-api/AdminBankAccount/org/global/action/CreateLinePaymentTxNotiApiKey/#{bankAccountId}"
param = {}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)
