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
apiUrl = "admin-api/AdminFinancialDoc/org/global/action/GetFinancialDocs"
param = {
  FullTextSearch: "",
  # FromDate: "2026-06-01T00:00:00",  # ใส่ถ้าต้องการกรองตามช่วงวันที่ (เทียบกับ CreatedDate)
  # ToDate: "2026-06-30T23:59:59",
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)

apiUrl = "admin-api/AdminFinancialDoc/org/global/action/GetFinancialDocCount"
result = make_request(:post, apiUrl, param)
puts(result)
