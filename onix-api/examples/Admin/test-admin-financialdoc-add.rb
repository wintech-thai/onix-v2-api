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

now = Time.now

###
apiUrl = "admin-api/AdminFinancialDoc/org/global/action/AddFinancialDoc"
param =  {
  # DocumentNo ไม่ต้องส่งมาก็ได้ ระบบจะ auto-generate ให้เป็น FD-YYYYMMDD-HHMM
  Description: "สรุปผลประกอบการเดือน #{now.strftime("%B %Y")}",
  Tags: "Test,Monthly",
  FromDate: Time.new(now.year, now.month, 1, 0, 0, 0).strftime("%Y-%m-%dT%H:%M:%S"),
  ToDate: now.strftime("%Y-%m-%dT23:59:59"),

  RevenueItemsArr: [
    { Code: "PayInFee", Label: "Pay-In Fee", Amount: 125000 },
    { Code: "PayOutFee", Label: "Pay-Out Fee", Amount: 48000 },
  ],

  ExpenseItemsArr: [
    { Code: "BANK-FEE", Label: "ค่าธรรมเนียมธนาคาร", Amount: 8500 },
    { Code: "SMS-COST", Label: "ค่าใช้จ่าย SMS OTP", Amount: 3200 },
  ],

  # เอามาจาก master ref RefType = "ShareHolderRatio" ที่เลือกไว้
  SharingItemsArr: [
    { Code: "สืบพงษ์ มนต์สา", Label: "สืบพงษ์ มนต์สา", Percent: 70, Amount: 113610 },
    { Code: "นางสาวรุ่งนภา ใจดี", Label: "นางสาวรุ่งนภา ใจดี", Percent: 30, Amount: 48690 },
  ],
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)
