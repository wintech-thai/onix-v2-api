#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/InventoryItem/org/#{orgId}/action/AddInventoryItem"
param = {
  Code: "G-TOANO.LA-35A",
  ReferenceCode: "-",
  NameTh: "กาวลาเท็กซ์ TOA 1 กก. การติดไม้ กาวยาง",
  NameEn: "TOA Latex Glue 1kg",
  ItemType: "RawMaterial",
  Unit: "Bottle",
  ItemGroup: "พัสดุอุปกรณ์",
  Remark: "",
  MinimumQuantity: 10.0,
  Price: 120.0,
  IsVatIncluded: true,
}

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
