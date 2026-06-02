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
cfgId = '3a76bf0a-8ce2-4d04-818e-754e9a14eb85'

### 
apiUrl = "admin-api/AdminConfiguration/org/global/action/EnableConfigById/#{cfgId}"
#apiUrl = "admin-api/AdminConfiguration/org/global/action/DisableConfigById/#{cfgId}"
param = nil

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)
