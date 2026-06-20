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
apiUrl = "admin-api/AdminMasterRef/org/global/action/AddMasterRef"
param =  {
  Code: "EXP-TEST-#{hhmmss}",
  Description: "Test expense type created at #{hhmmss}",
  Tags: "Test",
  RefType: "ExpenseType",

  # ใช้กับ RefType = "ShareHolderRatio" เก็บเป็น array ของผู้ถือหุ้น
  DefinitionObj: [
    { Name: "สืบพงษ์ มนต์สา", Percent: 70 },
    { Name: "นางสาวรุ่งนภา ใจดี", Percent: 30 },
  ]
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)
