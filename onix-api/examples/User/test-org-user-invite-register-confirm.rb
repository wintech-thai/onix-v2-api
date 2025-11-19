#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require 'cgi'
require 'base64'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

url = "https://register-dev.please-scan.com/pjame16/user-invite-confirm/73512d27-f92c-41dc-b780-c91a2d08fce8?data=eyJFbWFpbCI6InRlc3RpbmdAeHlzYS5jb20iLCJVc2VyTmFtZSI6ImFkaXNvcm4ucCIsIlBhc3N3b3JkIjpudWxsLCJOYW1lIjpudWxsLCJMYXN0bmFtZSI6bnVsbCwiSW52aXRlZEJ5Ijoic2V1YnBvbmcubW9uIiwiT3JnVXNlcklkIjoiZTNlNDdiN2QtMzAyMy00Zjk4LThmYzktOTcwZmEzZjM2NTliIn0%3d"
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
  Name: "Supreecha",
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
