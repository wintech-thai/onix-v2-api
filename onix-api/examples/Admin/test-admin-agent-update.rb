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

hhmmss = Time.now.strftime("%H%M%S")
agentId = "198b743a-4579-41ee-853f-a748f6a40825" # เปลี่ยนเป็น API Key ID ที่ต้องการทดสอบ

### 
apiUrl = "admin-api/AdminAgent/org/global/action/UpdateAgentById/#{agentId}"
param =  {
  Code: "pjame-test-agent",
  Description: "เครื่องเอ๋#1",
  Tags: "เอ๋",

  # เอา pay in bank account, เป็น array
  BankAccountsSelectedObj: [
    {
      Id: '215cd2ab-602a-43f1-8770-80a4fc51adb3', #BankAccountId
      BankCode: 'KTB',
      AccountNumber: '09800127157',
      AccountName: 'ประกาพร มนต์สา',
      PromptPayId: '3361300083524',
    }
  ]
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)
