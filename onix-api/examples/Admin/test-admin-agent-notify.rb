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
apiUrl = "admin-api/AdminAgent/org/global/action/NotifyLineMessage/#{agentId}"
param =  {
  TestField1: "xxxxx",
  PaymentAmount: 654.86, #ต้อง match กับ payment request ก่อนหน้าด้วย
  RemainAmount: 0.00,
  TxType: "PayIn",
  SourceBankCode: "KTB",
  SourceBankAccountNo: "XX-0987",
  DestinationBankCode: "TMB",
  DestinationAccountNo: "XX-9032",
}

token = File.read(keyFile)

ENV['API_KEY'] = ENV['AGENT_NOTIFY_API_KEY'] # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
#ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)
