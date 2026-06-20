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
apiUrl = "admin-api/AdminMasterRef/org/global/action/GetMasterRefs"
param = {
  FullTextSearch: "",
  RefType: "ExpenseType", # ลองเปลี่ยนเป็น "ShareHolderRatio" เพื่อดึงอีกประเภท หรือเว้นว่างเพื่อดึงทั้งหมด
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)

apiUrl = "admin-api/AdminMasterRef/org/global/action/GetMasterRefCount"
result = make_request(:post, apiUrl, param)
puts(result)
