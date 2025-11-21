export type User =
{
  id: number;
  displayName: string;
  email: string;
  token: string;
}

export type LoginCreds = {
  email: string;
  password: string;
}

export type RegisterCreds = {
  displayName: string;
  email: string;
  password: string;
}
