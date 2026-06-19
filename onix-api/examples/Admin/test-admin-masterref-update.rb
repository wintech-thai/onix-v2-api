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
masterRefId = "198b743a-4579-41ee-853f-a748f6a40825" # เปลี่ยนเป็น Master Ref ID ที่ต้องการทดสอบ

###
apiUrl = "admin-api/AdminMasterRef/org/global/action/UpdateMasterRefById/#{masterRefId}"
param =  {
  Description: "Updated description at #{hhmmss}",
  Tags: "Updated",

  # ใช้กับ RefType = "ShareHolderRatio" เก็บเป็น array ของผู้ถือหุ้น
  # หมายเหตุ: Code และ RefType แก้ไม่ได้ — service จะ ignore สองฟีลด์นี้
  DefinitionObj: [
    { Name: "สืบพงษ์ มนต์สา", Percent: 60 },
    { Name: "นางสาวรุ่งนภา ใจดี", Percent: 40 },
  ]
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)
