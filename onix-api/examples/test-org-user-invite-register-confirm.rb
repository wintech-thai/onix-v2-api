#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require 'cgi'
require 'base64'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

url = "https://register-dev.please-scan.com/napbiotec/user-signup-confirm/65443d6f-3c4c-4fc5-bc81-ecb2d99c5672?data=eyJFbWFpbCI6InBqYW1lLmZiMTQxQGdtYWlsLmNvbSIsIlVzZXJOYW1lIjoicGphbWVuYWphMTQxIiwiUGFzc3dvcmQiOm51bGwsIk5hbWUiOm51bGwsIkxhc3RuYW1lIjpudWxsLCJJbnZpdGVkQnkiOiJhcGkiLCJPcmdVc2VySWQiOiI5ZGI2YWUwZS01NWEwLTQyOGMtYWM2My0yYzYxNTQ2Mzc4NTAifQ%3d%3d"
uri = URI.parse(url)

# แปลง query string เป็น hash
params = CGI.parse(uri.query)
data = params['data'].first

path = uri.path
parts = path.split('/').reject(&:empty?)

decoded = Base64.decode64(data)
dataObj = JSON.parse(decoded)

orgId = parts[0]
regType = parts[1]
token = parts[2]

api = "ConfirmNewUserInvitation"
if (regType != 'user-signup-confirm')
  api = "ConfirmExistingUserInvitation"
end

#puts(dataObj)
userName = dataObj['UserName']

param =  {
  Email: dataObj['Email'],
  UserName: "#{userName}",
  Password: "Abc12345$343#1",
  Name: "Seubpong",
  LastName: "Monsar",
  OrgUserId: dataObj['OrgUserId'],
}

#puts(param)
#puts(parts)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil

apiUrl = "api/Registration/org/#{orgId}/action/#{api}/#{token}/#{userName}"
#puts(apiUrl)

result = make_request(:post, apiUrl, param)
puts(result)
