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
agentId = '198b743a-4579-41ee-853f-a748f6a40825' #'00c52e50-15cc-429d-9ec4-ab93afc99057'

hhmmss = Time.now.strftime("%H%M%S")

### 
apiUrl = "admin-api/AdminAgent/org/global/action/NotifyLineMessage/#{agentId}"
param =  {
  sourceType: "NOTIFICATION",
  sourceKey: "jp.naver.line.android",
  sourceLabel: "LINE",
  title: "Krungthai Connext",
  text: "เงินเข้า: 12.66 บาท เข้าบัญชี XX7157 เมื่อ 16/06/6",
}

token = File.read(keyFile)

ENV['API_KEY'] = ENV['AGENT_NOTIFY_API_KEY'] # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
#ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)
