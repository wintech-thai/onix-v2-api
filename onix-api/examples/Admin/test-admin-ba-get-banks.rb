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
### ดึงชื่อ bank
apiUrl = "admin-api/AdminBankAccount/org/#{orgId}/action/GetAvailableBanks"
param = nil

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:get, apiUrl, param)
puts(result)
puts("====")

### ดึงชื่อ bank ที่ ณ ตอนนี้ระบบรองรับ QR scan
apiUrl = "admin-api/AdminBankAccount/org/#{orgId}/action/GetAvailableSupportQrBanks"
result = make_request(:get, apiUrl, param)
puts(result)
