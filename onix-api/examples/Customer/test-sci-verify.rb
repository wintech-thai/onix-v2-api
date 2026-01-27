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

### 
apiUrl = "/org/#{orgId}/Verify/JY000001/L9M66LL?member=true"
param = nil
token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = nil
ENV['PsMemberToken'] = token
ENV['API_HTTP_ENDPOINT'] = 'https://scan-dev.please-scan.com'
#puts("===[#{token}]")

result = make_request(:get, apiUrl, param)
puts(result)
