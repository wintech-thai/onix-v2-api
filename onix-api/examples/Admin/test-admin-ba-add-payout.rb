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
apiUrl = "admin-api/AdminBankAccount/org/global/action/AddBankAccount"
param =  {
  BankCode: "TMB",
  AccountNumber: "178-2-13845-45",
  AccountName: "ทดสอบ ลายไทยกุก",
  PromptPayId: "3100600229995",
  Tags: "Testing",
  AccountType: "PromptPay",
  AccountCategory: "PayOut",
  AccountLevel: "Selected",
  PayinMinAmount: 0,
  PayinMaxAmount: 0,
  PayoutMinAmount: 0,
  PayoutMaxAmount: 0,
  DailyQuota: 0,
  CurrentDailyPayinAmount: 0,
  CurrentDailyPayinCount: 0,
  DailyPayinCountQuota: 0,
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)
