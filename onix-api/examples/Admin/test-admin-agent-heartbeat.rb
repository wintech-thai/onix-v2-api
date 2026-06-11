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
agentId = 'cdf71ee4-fb16-48dd-987e-6b6db52fb34f' #'00c52e50-15cc-429d-9ec4-ab93afc99057'

hhmmss = Time.now.strftime("%H%M%S")

### 
apiUrl = "admin-api/AdminAgent/org/global/action/NotifyHeartbeat/#{agentId}"
param =  {
  CPU: "4",
  Memory: "32",
  OsVersion: "1.0.0",
  AppVersion: "0.0.13",
  Baterry: "50",
}

token = File.read(keyFile)

ENV['API_KEY'] = ENV['AGENT_NOTIFY_API_KEY'] # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
#ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)
