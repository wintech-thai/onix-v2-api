#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = 'ppm-alfa999'
keyFile = ".token"
merchantId = "cfd098d5-ed0b-4acd-a10e-9fbb9e3d42c0"

hhmmss = Time.now.strftime("%H%M%S")

### Get Payment Requests
apiUrl = "api/PaymentRequest/org/#{orgId}/action/SubmitPayOutRequest/#{merchantId}"
param =  {
  RefId: "PAYOUT-0001922-#{hhmmss}",
  QrProvider: "PP",
  RequestedAmount: 100,

  BankCode: "TMB",
  BankAccountNo: "909440293998",
  BankAccountName: "Sebastian Vettel",
  PromptPayId: "2100600339556",
  AccountType: "PromptPay",
}

token = File.read(keyFile)

ENV['API_KEY'] = ENV['PAYOUT_REQUEST_API_KEY'] # ถ้าใช้ API KEY ก็เซ็ตเป็นค่าเดิมที่อ่านมาจากไฟล์
#ENV['ACCESS_TOKEN'] = nil

puts("===== Submit Payment Request =====")
result = make_request(:post, apiUrl, param)
puts(result)
