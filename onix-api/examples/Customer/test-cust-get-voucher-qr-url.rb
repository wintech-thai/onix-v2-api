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
privilegeId = "6f4ee733-1b20-43ca-bade-1146d03d32d8"

### 
apiUrl = "customer-api/CustomerPrivilege/org/#{orgId}/action/GetVoucherVerifyQrUrl/#{privilegeId}"
param = nil
token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:get, apiUrl, param)
puts(result)
