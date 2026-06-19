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
financialDocId = "198b743a-4579-41ee-853f-a748f6a40825" # เปลี่ยนเป็น Financial Doc ID ที่ต้องการทดสอบ

###
apiUrl = "admin-api/AdminFinancialDoc/org/global/action/UpdateFinancialDocById/#{financialDocId}"
param =  {
  # หมายเหตุ: DocumentNo แก้ไม่ได้ — repository จะ ignore ฟีลด์นี้
  Description: "Updated description at #{hhmmss}",
  Tags: "Updated",
  FromDate: "2026-06-01T00:00:00",
  ToDate: "2026-06-30T23:59:59",

  RevenueItemsArr: [
    { Code: "PayInFee", Label: "Pay-In Fee", Amount: 130000 },
    { Code: "PayOutFee", Label: "Pay-Out Fee", Amount: 50000 },
  ],

  ExpenseItemsArr: [
    { Code: "BANK-FEE", Label: "ค่าธรรมเนียมธนาคาร", Amount: 9000 },
  ],

  SharingItemsArr: [
    { Code: "สืบพงษ์ มนต์สา", Label: "สืบพงษ์ มนต์สา", Percent: 70, Amount: 119700 },
    { Code: "นางสาวรุ่งนภา ใจดี", Label: "นางสาวรุ่งนภา ใจดี", Percent: 30, Amount: 51300 },
  ],
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)
