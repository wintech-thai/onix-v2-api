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
  AccountNumber: "178-2-13875-3",
  AccountName: "สรพงษ์ ไทยกุกๆๆๆ",
  PromptPayId: "3100600229875",
  Tags: "Testing",
  AccountType: "PromptPay",
  AccountCategory: "Transit",
  AccountLevel: "Global",
  PayinMinAmount: 10,
  PayinMaxAmount: 1000000,
  PayoutMinAmount: 10,
  PayoutMaxAmount: 1000000,
  DailyQuota: 1000000,
  CurrentDailyPayinAmount: 0,
  CurrentDailyPayinCount: 0,
  DailyPayinCountQuota: 1000,
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)
