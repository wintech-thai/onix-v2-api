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

### 
apiUrl = "admin-api/AdminAgent/org/global/action/AddLineApiAgent"
param =  {
  Code: "Test Agent #{hhmmss}",
  Description: "Test agent created at #{hhmmss}",
  Tags: "Test",

  # เอา pay in bank account, เป็น array
  BankAccountsSelectedObj: [
    {
      Id: '198b743a-4579-41ee-853f-a748f6a40825', #BankAccountId
      BankCode: 'KTB',
      AccountNumber: '88099309292029',
      AccountName: 'สืบศักดิ์ มารกุ๊กกู๋',
      PromptPayId: '0903990990',
    }
  ],

  AgentConfigObj: {
    UserName: 'pjamexxxx',
    ApiKey: 'This is API key xxxx',
  }
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)
