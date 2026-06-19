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
# ใช้ตอนกดปุ่ม "คำนวณรายรับ" ในหน้าจอ Financial Document — ไปเรียก GetRevenueSummary
# ของ ISummaryService มาให้ ได้ TotalPayInFee / TotalPayOutFee สำหรับช่วงเวลาที่เลือก
apiUrl = "admin-api/AdminFinancialDoc/org/global/action/CalculateRevenue"
param = {
  FromDate: "2026-06-01T00:00:00",
  ToDate: "2026-06-30T23:59:59",
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)
