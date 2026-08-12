#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

# ทดสอบขอเลขที่เอกสาร (GetDocumentNumber)
# เรียกซ้ำหลายรอบเพื่อทดสอบว่า sequence เพิ่มขึ้นถูกต้อง
docType = "TempDocumentNo"

3.times do |i|
  apiUrl = "api/DocumentNumber/org/#{orgId}/action/GetDocumentNumber/#{docType}"
  result = make_request(:get, apiUrl, nil)
  puts("Call #{i + 1}: #{result.to_json}")
end
